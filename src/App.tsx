import { useLocation } from "react-router-dom";
import { TooltipProvider } from "./components/ui/tooltip";
import { Toaster } from "./components/ui/sonner";
import Main from "./partials/Main";

export default function App() {
  // scroll to top when the route changes
  useLocation();
  window.scrollTo({ top: 0, left: 0, behavior: "instant" });

  return (
    <TooltipProvider>
      <div className="max-w-full overflow-x-hidden">
        <Main />
        <Toaster />
      </div>
    </TooltipProvider>
  );
}
