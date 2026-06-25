import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";

export const NotFoundPage = () => {
  return (
    <div className="flex h-full flex-col items-center justify-center gap-4">
      <div className="text-8xl font-bold">404</div>

      <h1 className="text-2xl font-semibold">페이지를 찾을 수 없습니다</h1>

      <p className="text-muted-foreground">
        요청한 페이지가 존재하지 않거나 이동되었습니다.
      </p>

      <Button asChild>
        <Link to="/">홈으로 이동</Link>
      </Button>
    </div>
  );
};
