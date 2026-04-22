import { useGetApiTerritories } from "@/api/generated/territories/territories";
import { useEffect } from "react";
import { toast } from "sonner";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"

GamePage.route = {
  path: "/game",
};

export default function GamePage() {
  
  const { data, isLoading, isError } = useGetApiTerritories();

  
  console.log("data", data);
  console.log("isLoading", isLoading);
  console.log("isError", isError);

  if(isError){
    toast.error("Error fetching Territories");
  }

  const territories = Array.isArray(data)
    ? data
    : Array.isArray(data?.data)
    ? data.data
    : [];

  useEffect(() => {
  if (isLoading || territories.length === 0) {
    toast.warning("Fetching Territories");

    // Delay the reload slightly so the toast can show
    const timeout = setTimeout(() => {
      window.location.reload();
    }, 4500);

    return () => clearTimeout(timeout);
  }
}, [isLoading, territories]);

  let currentRow: any[] = [];

  if(!isError && !isLoading){
    //Todo: load up player territories once created, and assign their data to each territory
    return (
      <div className="min-h-screen w-full bg-slate-100 p-6">
        <div className="container mx-auto">
          <div className="text-center text-3xl font-bold mb-6">
            <h1>Typing Territory</h1>
          </div>

          <div className="flex gap-6">
            {/* Game Map */}
            <div className="flex flex-col bg-cover bg-center bg-no-repeat" style={{ backgroundSize:  "auto 100%", backgroundImage: "url('/terrain_background.png')" }}>
              {territories.map((t, i) => {
                currentRow.push(t);

                const isRowEnd = t.eastAdjacentId === -1;

                if (isRowEnd) {
                  const rowToRender = currentRow;
                  currentRow = []; // reset for next row

                  return (
                    <div
                      key={`row-${i}`}
                      className="flex gap-4 mb-4 snap-center translate-x-9 md:translate-x-0"
                    >
                      {rowToRender.map(item => (
                        <Card key={item.id} className="w-32 h-32 bg-white/60 backdrop-blur-sm">
                          <CardHeader>
                            <CardTitle className="break-words text-xs line-clamp-2 text-black">{item.name}</CardTitle>
                          </CardHeader>
                          <CardContent>
                            <p className="text-xs text-black" >North: {item.northAdjacentId}</p>
                          </CardContent>
                        </Card>
                      ))}
                    </div>
                  );
                }
              })}
            </div>
            {/* Game Decisions */}
            <div className="flex flex-col items-center w-80 p-4 border rounded">
              <p>Game Status</p>
            </div>
          </div>
        </div>
      </div>
    );
  }
}
