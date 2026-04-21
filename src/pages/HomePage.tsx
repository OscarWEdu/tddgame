import {
  useGetApiGameSession,
  usePostApiGameSession,
} from "../api/generated/game-sessions/game-sessions";

HomePage.route = {
  path: "/",
};

export default function HomePage() {
  const { data, isLoading, isError } = useGetApiGameSession();

  const gameSessionMutation = usePostApiGameSession();

  console.log("data", data);
  console.log("isLoading", isLoading);
  console.log("isError", isError);
  return (
    <div className="container mx-auto">
      <h1>Hello World</h1>
      <button
        title="Click me!"
        onClick={() =>
          gameSessionMutation.mutate({ data: { name: "My new game!" } })
        }
      >
        <span>Click me!</span>
      </button>
    </div>
  );
}
