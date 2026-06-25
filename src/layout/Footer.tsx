import { ThemeToggle } from "@/components/ThemeToggle";

export const Footer = () => {
  return (
    <footer className="flex justify-between items-center text-sm border-t-1">
      <nav className="flex gap-2 p-1 items-center">
        <ThemeToggle />
        <a href="/chat">Chat</a>
        <a href="/settings">Settings</a>
      </nav>
      <nav className="flex gap-2 p-1 items-center">
        <a href="/">Home</a>
        <a href="/chat">Chat</a>
        <a href="/settings">Settings</a>
      </nav>
    </footer>
  );
};
