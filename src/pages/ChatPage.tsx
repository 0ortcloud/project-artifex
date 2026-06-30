import { useEffect, useState, useRef } from "react";
import { ArrowUp, Ellipsis, MessageCircleMore } from "lucide-react";
import { toast } from "sonner";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
import rehypeHighlight from "rehype-highlight";
import "katex/dist/katex.min.css";

import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from "@/components/ui/resizable";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Textarea } from "@/components/ui/textarea";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Spinner } from "@/components/ui/spinner";

export const ChatPage = () => {
  const [sessionId, setSessionId] = useState<number>(0);
  const [chatSessionList, setChatSessionList] = useState<ChatSessionMini[]>([]);
  const [selectedSession, setSelectedSession] =
    useState<ChatSessionMini | null>(null);
  const [sendText, setSendText] = useState<string>("");
  const [chatReport, setChatReport] = useState<Chat[]>([]);
  const [rewriteTitle, setRewriteTitle] = useState<string>("");
  const [open, setOpen] = useState<boolean>(false);
  const [isStreaming, setIsStreaming] = useState<boolean>(false);

  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  const bringMySessionList = async () => {
    try {
      const response = await fetch("/api/chat/session/list", { method: "GET" });
      const data: ChatSessionMini[] = await response.json();

      if (data.length !== 0) {
        setChatSessionList(data);
      }
    } catch (error) {
      console.error("Failed to fetch session list:", error);
    }
  };

  const bringMyChat = async (id: number) => {
    try {
      const response = await fetch(`/api/chat/session/${id}`, {
        method: "GET",
      });
      const data: Chat[] = await response.json();
      setChatReport(data);
    } catch (error) {
      console.error("Failed to fetch chat logs:", error);
    }
  };

  const sendMyText = async (currentSessionId: number, text: string) => {
    if (!text.trim()) {
      toast.error("テキストを入力してください。");
      return;
    }

    setIsStreaming(true);

    const userChatData: Chat = {
      id: Date.now(),
      sessionId: currentSessionId,
      messageRole: 1,
      content: text,
      score: 0,
      createdAt: Math.floor(Date.now() / 1000),
      toolName: 0,
    };

    const aiChatPlaceholderId = Date.now() + 1;
    const aiChatData: Chat = {
      id: aiChatPlaceholderId,
      sessionId: currentSessionId,
      messageRole: 2,
      content: "",
      score: 0,
      createdAt: Math.floor(Date.now() / 1000),
      toolName: 0,
    };

    setChatReport((prev) => [...prev, userChatData, aiChatData]);

    try {
      const response = await fetch(`/api/chat`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          sessionId: currentSessionId,
          messageRole: 1,
          content: text,
          score: 0,
          toolName: 0,
        }),
      });

      if (!response.ok || !response.body) {
        toast.error("送信失敗");
        setIsStreaming(false);
        return;
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder("utf-8");
      let accumulatedAnswer = "";

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        const lines = chunk.split("\n");

        for (const line of lines) {
          if (!line.startsWith("data: ")) continue;

          const jsonStr = line.replace("data: ", "").trim();
          if (!jsonStr) continue;

          try {
            const parsed = JSON.parse(jsonStr);

            if (parsed.SessionId && currentSessionId === 0) {
              setSessionId(parsed.SessionId);
              bringMySessionList();
            }

            if (parsed.AnswerChunk) {
              accumulatedAnswer += parsed.AnswerChunk;

              setChatReport((prev) => {
                const newReport = [...prev];
                const aiChatIndex = newReport.findIndex(
                  (chat) => chat.id === aiChatPlaceholderId,
                );

                if (aiChatIndex !== -1) {
                  newReport[aiChatIndex] = {
                    ...newReport[aiChatIndex],
                    content: accumulatedAnswer,
                  };
                }
                return newReport;
              });
            }
          } catch (e) {
            console.error("JSONパースエラー:", e);
          }
        }
      }
    } catch (error) {
      console.error("ストリーミング通信エラー:", error);
      toast.error("通信中にエラーが発生しました");
    } finally {
      setIsStreaming(false);
    }
  };

  const rewriteChatSessionTitle = async (sessionId: number, title: string) => {
    setOpen(false);
    try {
      const response = await fetch(`/api/chat/session/${sessionId}`, {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ title }),
      });
      const data: boolean = await response.json();

      if (data) {
        bringMySessionList();
        toast.success("修正成功");
      } else {
        toast.error("修正失敗");
      }
    } catch (error) {
      toast.error("修正中にエラーが発生しました");
    }
  };

  const removeOneSession = async (sessionId: number) => {
    try {
      const response = await fetch(`/api/chat/session/${sessionId}`, {
        method: "DELETE",
      });
      const data: boolean = await response.json();

      if (data) {
        bringMySessionList();
        toast.success("セッション削除成功");
      } else {
        toast.error("セッション削除失敗");
      }
    } catch (error) {
      toast.error("削除中にエラーが発生しました");
    }
  };

  useEffect(() => {
    bringMySessionList();
  }, []);

  useEffect(() => {
    scrollToBottom();
  }, [chatReport, isStreaming]);

  return (
    <>
      <ResizablePanelGroup orientation="horizontal" className="h-full">
        {/* サイドバー: セッション一覧 */}
        <ResizablePanel defaultSize="15%" minSize="10%" maxSize="50%">
          <ScrollArea className="p-4 h-full">
            <div className="flex justify-between items-center mb-4">
              <h4 className="text-sm leading-none font-medium">Session List</h4>
              <Button
                onClick={() => {
                  setSessionId(0);
                  setChatReport([]);
                }}
                variant="secondary"
              >
                New Session
              </Button>
            </div>
            <div className="flex flex-col gap-1">
              {chatSessionList.map((value, i) => (
                <div
                  key={i}
                  className={`flex justify-between items-center rounded-sm px-2 py-1 text-xs hover:bg-primary-foreground cursor-pointer ${
                    sessionId === value.id ? "bg-primary-foreground" : ""
                  }`}
                  onClick={() => {
                    if (isStreaming) {
                      toast.warning("AIが応答中です。しばらくお待ちください。");
                      return;
                    }
                    setSessionId(value.id);
                    bringMyChat(value.id);
                  }}
                >
                  <div className="p-1 text-ellipsis overflow-hidden whitespace-nowrap">
                    {value.title}
                  </div>
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="icon" className="h-4 w-4">
                        <Ellipsis className="h-3 w-3" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent>
                      <DropdownMenuGroup>
                        <DropdownMenuLabel>編集</DropdownMenuLabel>
                        <DropdownMenuItem
                          onClick={(e) => {
                            e.stopPropagation();
                            setSelectedSession(value);
                            setRewriteTitle(value.title);
                            setOpen(true);
                          }}
                        >
                          タイトル修正
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          className="text-destructive"
                          onClick={(e) => {
                            e.stopPropagation();
                            setSelectedSession(value);
                            removeOneSession(value.id);
                          }}
                        >
                          削除
                        </DropdownMenuItem>
                      </DropdownMenuGroup>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
              ))}
            </div>
          </ScrollArea>
        </ResizablePanel>

        <ResizableHandle />

        {/* メインコンテンツ: チャットエリア */}
        <ResizablePanel>
          <ResizablePanelGroup orientation="vertical">
            {/* メッセージ表示エリア */}
            <ResizablePanel defaultSize="80%" minSize="10%" maxSize="85%">
              {chatReport.length !== 0 ? (
                <div className="p-4 flex flex-col gap-4 overflow-y-auto h-full">
                  {chatReport.map((value, i) => {
                    const isLastMessage = i === chatReport.length - 1;
                    const isAiMessage = value.messageRole === 2;

                    return (
                      <div
                        key={i}
                        className={`flex w-full ${
                          value.messageRole === 1
                            ? "justify-end"
                            : "justify-start"
                        }`}
                      >
                        <div
                          className={`max-w-[85%] rounded-lg px-4 py-3 text-sm shadow-sm ${
                            value.messageRole === 1
                              ? "bg-blue-600 text-white"
                              : "bg-gray-100 text-gray-900 dark:bg-zinc-800 dark:text-gray-100"
                          }`}
                        >
                          {isAiMessage &&
                          isLastMessage &&
                          !value.content &&
                          isStreaming ? (
                            <div className="flex items-center gap-2 py-1">
                              <Spinner />
                              <span className="text-xs text-gray-500">
                                Thinking...
                              </span>
                            </div>
                          ) : (
                            <div
                              className={
                                value.messageRole === 2
                                  ? "prose prose-sm dark:prose-invert max-w-none break-words"
                                  : "whitespace-pre-wrap break-words"
                              }
                            >
                              {value.messageRole === 2 ? (
                                <ReactMarkdown
                                  remarkPlugins={[remarkGfm, remarkMath]}
                                  rehypePlugins={[rehypeHighlight, rehypeKatex]}
                                >
                                  {value.content}
                                </ReactMarkdown>
                              ) : (
                                value.content
                              )}
                            </div>
                          )}
                        </div>
                      </div>
                    );
                  })}
                  <div ref={messagesEndRef} />
                </div>
              ) : (
                <Empty>
                  <EmptyHeader>
                    <EmptyMedia variant="icon">
                      <MessageCircleMore />
                    </EmptyMedia>
                    <EmptyTitle>新しい話題で話しましょう！</EmptyTitle>
                    <EmptyDescription>
                      どんな楽しい会話ができるんですかね。 ワクワクします！
                    </EmptyDescription>
                  </EmptyHeader>
                </Empty>
              )}
            </ResizablePanel>

            <ResizableHandle />

            {/* 入力エリア */}
            <ResizablePanel>
              <div className="flex h-full p-4 gap-2">
                <Textarea
                  placeholder="メッセージを入力してください..."
                  value={sendText}
                  onChange={(e) => setSendText(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" && !e.shiftKey) {
                      e.preventDefault();
                      if (isStreaming) return;
                      sendMyText(sessionId, sendText);
                      setSendText("");
                    }
                  }}
                  disabled={isStreaming}
                  className="
                    font-terminal h-full resize-none border-0
                    text-zinc-900 bg-white placeholder:text-zinc-400
                    dark:bg-black dark:text-hacker-text
                    dark:placeholder:text-hacker-text-placeholder
                    focus-visible:ring-0 focus-visible:ring-offset-0
                  "
                />
                <Button
                  variant="secondary"
                  size="icon"
                  disabled={isStreaming || !sendText.trim()}
                  onClick={() => {
                    sendMyText(sessionId, sendText);
                    setSendText("");
                  }}
                >
                  <ArrowUp />
                </Button>
              </div>
            </ResizablePanel>
          </ResizablePanelGroup>
        </ResizablePanel>
      </ResizablePanelGroup>

      {/* ダイアログ: セッション名変更 */}
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="mb-4">セッション名変更</DialogTitle>
            <Input
              value={rewriteTitle}
              onChange={(e) => setRewriteTitle(e.target.value)}
              className="mb-4"
            />
            <div className="flex justify-end">
              <Button
                onClick={() => {
                  if (!selectedSession) return;
                  rewriteChatSessionTitle(selectedSession.id, rewriteTitle);
                }}
              >
                確認
              </Button>
            </div>
          </DialogHeader>
        </DialogContent>
      </Dialog>
    </>
  );
};
