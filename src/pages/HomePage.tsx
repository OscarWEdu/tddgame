import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import {
  useGetApiGameSession,
  useGetApiGameSessionId,
  usePostApiGameSession,
} from "../api/generated/game-sessions/game-sessions";

HomePage.route = {
  path: "/",
};

export default function HomePage() {
  const { data, isLoading, isError } = useGetApiGameSession();

  const { data: gameSessionById } = useGetApiGameSessionId(
    "550e8400-e29b-41d4-a716-446655440000",
  );

  console.log("gameSessionById", gameSessionById);

  const gameSessionMutation = usePostApiGameSession({
    mutation: {
      onSuccess: (response) => {
        if (response.status >= 400) {
          toast.error("Error!!!!!");
        } else {
          toast.success("Let's goooo");
        }
      },
    },
  });

  console.log("data", data);
  console.log("isLoading", isLoading);
  console.log("isError", isError);
  return (
    <div className="container mx-auto">
      <h1>Hello World</h1>
      <Button
        className="bg-success"
        onClick={() => {
          toast.success("Let's goooo!");
        }}
      >
        <span>Success</span>
      </Button>
      <Button
        onClick={() => {
          toast.info("Important info!");
        }}
      >
        <span>Info</span>
      </Button>
      <Button
        className="bg-warning"
        onClick={() => {
          toast.warning("Oh Warning!");
        }}
      >
        <span>Warning</span>
      </Button>
      <Button
        className="bg-destructive"
        onClick={() => {
          toast.error("Oh nooooo!");
        }}
      >
        <span>Error</span>
      </Button>
    </div>
  );
}
