import { BookOpenIcon } from "lucide-react";
import { Outlet, useLocation } from "react-router-dom";
import { Toaster } from "./components/ui/sonner";
import { TooltipProvider } from "./components/ui/tooltip";

export default function App() {
  useLocation();
  window.scrollTo({ top: 0, left: 0, behavior: "instant" });

  return (
    <TooltipProvider>
      <div className="fixed inset-0 -z-10 bg-[url('/background.jpg')] bg-cover bg-center bg-no-repeat" />
      <div className="flex min-h-screen flex-col overflow-x-hidden">
        <header className="flex items-center justify-between px-6 py-4">
          <span className="font-mono text-lg font-bold tracking-tight text-primary-foreground">
            TDDGame
          </span>
          <a
            href="https://github.com/OscarWEdu/tddgame/wiki/Standard-RISK-Rules"
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-foreground"
          >
            <BookOpenIcon className="size-4" />
            How to play
          </a>
        </header>
        <main className="flex flex-1 flex-col container mx-auto px-4">
          <Outlet />
        </main>
      </div>
      <Toaster />
    </TooltipProvider>
  );
}
