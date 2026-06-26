import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";

export const NotFoundPage = () => {
  return (
    <div className="flex h-full flex-col items-center justify-center gap-4">
      <div className="text-8xl font-bold">404</div>

      <h1 className="text-2xl font-semibold">ページが見つかりません</h1>

      <p className="text-muted-foreground">
        お探しのページは存在しないか、移動した可能性があります。
      </p>

      <Button asChild>
        <Link to="/">ホームに移動</Link>
      </Button>
    </div>
  );
};
