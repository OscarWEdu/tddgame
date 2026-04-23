import { useParams } from "react-router-dom";

import { useGetApiTerritories } from "@/api/generated/territories/territories";
import RiskMap from "@/components/RiskMap";
import { useEffect } from "react";
import { toast } from "sonner";

GamePage.route = {
  path: "/game/:sessionId",
};

export default function GamePage() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const { data, isLoading, isError } = useGetApiTerritories();

  const territories = data?.data ?? [];

  useEffect(() => {
    if (isLoading || territories.length === 0) {
      toast.warning("Fetching Territories");

      const timeout = setTimeout(() => {
        window.location.reload();
      }, 4500);

      return () => clearTimeout(timeout);
    }
  }, [isLoading, territories.length]);
  
    return (
    <div className="min-h-screen w-full bg-slate-100 p-6">
      <div className="container mx-auto">
        <div className="text-center text-3xl font-bold mb-6">
          <h1>Typing Territory</h1>
        </div>
        <p className="mt-2 text-slate-600">Session id: {sessionId}</p>

        <div className="flex gap-6">
          {/* Game Map */}
          <div className="flex-1 rounded border bg-white p-2">
            <RiskMap territories={territories} />
          </div>
          {/* Game Decisions */}
          <div className="flex w-80 flex-col gap-2 rounded border bg-white p-4">
            <p className="font-semibold">Game Status</p>
            {isLoading && <p className="text-sm text-slate-500">Loading territories…</p>}
            {isError && <p className="text-sm text-red-600">could not get territories</p>}
            {!isLoading && !isError && (
              <p className="text-sm text-slate-500">
                {territories.length} territories loaded
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
