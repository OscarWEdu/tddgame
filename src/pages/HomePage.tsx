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
      onError: () => {
        return toast.error("Error!!!!!");
      },
    },
  });

  console.log("data", data);
  console.log("isLoading", isLoading);
  console.log("isError", isError);
  return (
    <div className="container mx-auto">
      <h1>Hello World</h1>
      <button
        title="Click me!"
        onClick={() => gameSessionMutation.mutate({ data: { name: "" } })}
      >
        <span>Click me!</span>
      </button>
    </div>
  );
}
