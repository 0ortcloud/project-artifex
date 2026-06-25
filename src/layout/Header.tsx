export const Header = () => {
  return (
    <header className="">
      <nav className="flex gap-2 p-1 items-center text-sm border-b-1">
        <a href="/">
          <img src="/public/icon.png" width={24} height={24} />
        </a>
        <a href="/chat">Chat</a>
        <a href="/settings">Settings</a>
      </nav>
    </header>
  );
};
