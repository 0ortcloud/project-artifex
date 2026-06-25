import { Button } from "@/components/ui/button";
import { Link } from "react-router-dom";

export const HomePage = () => {
  return (
    <>
      <Button asChild>
        <Link to="/chat">챗으로 이동</Link>
      </Button>
    </>
  );
};
