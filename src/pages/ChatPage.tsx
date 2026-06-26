import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from "@/components/ui/resizable";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Textarea } from "@/components/ui/textarea";
import { useEffect, useState } from "react";
// import { config } from "@/lib/config";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { ArrowUp, Ellipsis, MessageCircleMore } from "lucide-react";
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

export const ChatPage = () => {
  const [sessionId, setSessionId] = useState<number>(0);
  const [chatSessionList, setChatSessionList] = useState<ChatSessionMini[]>([]);
  const [selectedSession, setSelectedSession] =
    useState<ChatSessionMini | null>(null);
  const [sendText, setSendText] = useState<string>("");
  const [chatReport, setChatReport] = useState<Chat[]>([]);
  const [rewriteTitle, setRewriteTitle] = useState<string>("");
  const [open, setOpen] = useState<boolean>(false);

  const bringMySessionList = async () => {
    const response = await fetch("/api/chat/session/list", {
      method: "GET",
    });
    const data: ChatSessionMini[] = await response.json();
    console.log(data);
    if (data.length !== 0) {
      setChatSessionList(data);
    }
  };

  const bringMyChat = async (id: number) => {
    const response = await fetch(`/api/chat/session/${id}`, {
      method: "GET",
    });
    const data: Chat[] = await response.json();
    setChatReport(data);
  };

  const sendMyText = async (sessionId: number, text: string) => {
    if (!text.trim()) {
      toast.error("テキストを入力してください。");
      return;
    }
    const send = {
      sessionId: sessionId,
      messageRole: 1,
      content: text,
      score: 0,
      toolName: 0,
    };
    const rinjidata: Chat = {
      id: 0,
      sessionId: sessionId,
      messageRole: 1,
      content: text,
      score: 0,
      createdAt: 0,
      toolName: 0,
    };
    setChatReport((prev) => [...prev, rinjidata]);
    const response = await fetch(`/api/chat`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(send),
    });
    const data: Chat | boolean = await response.json();
    setChatReport((prev) => prev.slice(0, -1));
    if (data === false) {
      toast.error("送信失敗");
    } else if (typeof data !== "boolean") {
      console.log(data);
      setChatReport((prev) => [...prev, data]);
    }
    return;
  };

  const rewriteChatSessionTitle = async (sessionId: number, title: string) => {
    setOpen(false);
    const response = await fetch(`/api/chat/session/${sessionId}`, {
      method: "PATCH",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        title,
      }),
    });
    const data: boolean = await response.json();

    if (data) {
      bringMySessionList();
      toast.success("修正成功");
    } else {
      toast.error("修正失敗");
    }
    return;
  };

  const removeOneSession = async (sessionId: number) => {
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
  };

  useEffect(() => {
    bringMySessionList();
    if (sessionId != 0) bringMyChat(sessionId);
  }, [sessionId]);

  return (
    <>
      <ResizablePanelGroup orientation="horizontal" className="h-full">
        <ResizablePanel defaultSize="15%" minSize="10%" maxSize="50%">
          <ScrollArea className="p-4">
            <div className="flex justify-between items-center">
              <h4 className="mb-4 text-sm leading-none font-medium">
                Session List
              </h4>
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
                  className="flex justify-between items-center rounded-sm px-2 py-1 text-xs hover:bg-primary-foreground"
                  onClick={() => {
                    setSessionId(value.id);
                  }}
                >
                  <div className="p-1 whitespace-nowrap text-ellipsis whitespace-nowrap">
                    {value.title}
                  </div>
                  <DropdownMenu>
                    <DropdownMenuTrigger>
                      <Ellipsis />
                    </DropdownMenuTrigger>
                    <DropdownMenuContent>
                      <DropdownMenuGroup>
                        <DropdownMenuLabel>編集</DropdownMenuLabel>
                        <DropdownMenuItem
                          onClick={() => {
                            setSelectedSession(value);
                            setRewriteTitle(value.title);
                            setOpen(true);
                          }}
                        >
                          タイトル修正
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          onClick={() => {
                            setSelectedSession(value);
                            removeOneSession(sessionId);
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

        <ResizablePanel>
          <ResizablePanelGroup orientation="vertical">
            <ResizablePanel defaultSize="80%" minSize="10%" maxSize="85%">
              {chatReport.length !== 0 ? (
                <div className="p-4 flex flex-col gap-2">
                  {chatReport.map((value, i) => (
                    <div
                      key={i}
                      className={`flex w-full ${
                        value.messageRole === 1
                          ? "justify-end"
                          : "justify-start"
                      }`}
                    >
                      <div
                        className={`max-w-[70%] rounded px-3 py-2 text-sm ${
                          value.messageRole === 1
                            ? "bg-blue-500 text-white"
                            : "bg-gray-200 text-black"
                        }`}
                      >
                        {value.content}
                      </div>
                    </div>
                  ))}
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
            <ResizablePanel>
              <div className="flex h-full p-4">
                <Textarea
                  placeholder="メッセージを入力してください..."
                  value={sendText}
                  onChange={(e) => setSendText(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" && !e.shiftKey) {
                      e.preventDefault();
                      sendMyText(sessionId, sendText);
                      setSendText("");
                    }
                  }}
                  className="
                  font-terminal h-full resize-none border-0
                  text-zinc-900 bg-white placeholder:text-zinc-400
                  dark:bg-black dark:text-hacker-text
                  dark:placeholder:text-hacker-text-placeholder
                  focus-visible:ring-0 focus-visible:ring-offset-0
                "
                />
                <Button
                  variant={"secondary"}
                  size={"icon"}
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
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>セッション名変更</DialogTitle>

            <Input
              value={rewriteTitle}
              onChange={(e) => setRewriteTitle(e.target.value)}
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
