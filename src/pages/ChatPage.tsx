import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from "@/components/ui/resizable";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Textarea } from "@/components/ui/textarea";
import { useEffect, useState } from "react";
// import { config } from "@/lib/config";

export const ChatPage = () => {
  const [sessionId, setSessionId] = useState<number>(0);
  const [chatSessionList, setChatSessionList] = useState<ChatSessionMini[]>([]);
  const [sendText, setSendText] = useState<string>("");
  const [chatReport, setChatReport] = useState<Chat[]>([]);

  const bringMySessionList = async () => {
    const response = await fetch("/api/chat/session/list", {
      method: "GET",
    });
    const data: ChatSessionMini[] = await response.json();
    setChatSessionList(data);
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
      alert("텍스트를 입력하세요.");
      return;
    }
    const response = await fetch(`/api/chat`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        sessionId,
        text,
      }),
    });
    const data = await response.json();
    console.log(data);
  };

  useEffect(() => {
    bringMySessionList();
    bringMyChat(sessionId);
  }, [sessionId]);

  return (
    <ResizablePanelGroup orientation="horizontal" className="h-full">
      <ResizablePanel defaultSize="15%" minSize="10%" maxSize="50%">
        <ScrollArea className="p-4">
          <h4 className="mb-4 text-sm leading-none font-medium">대화 세션</h4>
          <div className="flex flex-col gap-1">
            {chatSessionList.map((value, i) => (
              <div
                key={i}
                onClick={() => {
                  setSessionId(value.id);
                }}
                className="p-1"
              >
                {value.title}
              </div>
            ))}
            {sessionId}
          </div>
        </ScrollArea>
      </ResizablePanel>

      <ResizableHandle />

      <ResizablePanel>
        <ResizablePanelGroup orientation="vertical">
          <ResizablePanel defaultSize="80%" minSize="10%" maxSize="85%">
            <div className="p-4 flex flex-col gap-2">
              {chatReport.map((value, i) => (
                <div key={i} className="">
                  {value.content}
                </div>
              ))}
            </div>
          </ResizablePanel>
          <ResizableHandle />
          <ResizablePanel>
            <div className="flex h-full flex-col p-4">
              <Textarea
                placeholder="메시지를 입력하세요..."
                onChange={(e) => setSendText(e.target.value)}
                className="
            font-terminal
  h-full
  resize-none
  border-0

  bg-white
  text-zinc-900
  placeholder:text-zinc-400

  dark:bg-black
  dark:text-hacker-text
  dark:placeholder:text-hacker-text-placeholder

  focus-visible:ring-0
  focus-visible:ring-offset-0
          "
              />
            </div>
            <button onClick={() => sendMyText(sessionId, sendText)}>
              전송
            </button>
          </ResizablePanel>
        </ResizablePanelGroup>
      </ResizablePanel>
    </ResizablePanelGroup>
  );
};
