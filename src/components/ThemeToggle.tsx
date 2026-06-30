"use client";

import { useEffect, useState } from "react"; // client side マウント状態を管理するために追加
import { Moon, Sun } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useTheme } from "next-themes";

export const ThemeToggle = () => {
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = useState<boolean>(false);

  // クライアント側にマウントされるまで待機（ハイドレーション不一致の防止）
  useEffect(() => {
    setMounted(true);
  }, []);

  // マウントされる前は初期テーマの混乱を防ぐため、ボタンの形だけ維持（またはプレースホルダーを返す）
  if (!mounted) {
    return (
      <Button variant="outline" size="icon" disabled>
        <span className="h-4 w-4" />
      </Button>
    );
  }

  return (
    <Button
      variant="outline"
      size="icon"
      onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
    >
      {theme === "dark" ? (
        <Sun className="h-4 w-4" />
      ) : (
        <Moon className="h-4 w-4" />
      )}
    </Button>
  );
};
