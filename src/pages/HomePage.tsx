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
    <>
      <div className="fixed inset-0 -z-10 bg-[url('/background.jpg')] bg-cover bg-center bg-no-repeat" />
      <h1>Hello World</h1>
      <button
        title="Click me!"
        onClick={() =>
          gameSessionMutation.mutate({ data: { name: "My new game!" } })
        }
      >
        <span>Click me!</span>
      </button>
    </>
  );
}
