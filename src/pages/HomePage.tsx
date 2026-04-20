import { useEffect, useState } from "react";

type GameSession = {
  id: number;
  name: string;
  status: string;
};

HomePage.route = {
  path: '/'
};

export default function HomePage() {
  const [sessions, setsessions] = useState<GameSession[]>([]);   // useState är en React Hook som låter oss lägga till state i en funktionell komponent. Vi använder den för att skapa en state-variabel "sessions" som är en array av GameSession-objekt, och en funktion "setsessions" som vi kan använda för att uppdatera den variabeln.
  const [loading, setLoading] = useState(true);          // Vi skapar också en state-variabel "loading" som är en boolean, och en funktion "setLoading" som vi kan använda för att uppdatera den variabeln. Vi sätter initialt loading till true, vilket betyder att vi är i ett laddningstillstånd.

  useEffect(() => {
    fetch("/api/game-session")     // en React Hook som låter oss utföra biverkningar i en funktionell komponent. Vi använder den för att hämta data från vår backend när komponenten mountas. Vi skickar en tom array som andra argument, vilket betyder att denna effekt bara kommer att köras en gång, när komponenten först renderas.
      .then((res) => res.json())        // När vi får ett svar från fetch, konverterar vi det till JSON-format.
      .then((data) => {               // När vi har datan, uppdaterar vi vår "sessions" state-variabel med den data vi fick från backend, och sätter loading till false, vilket betyder att vi inte längre är i ett laddningstillstånd.
        setsessions(data);            // Vi uppdaterar vår "sessions" state-variabel med den data vi fick från backend.

        setLoading(false);      // Vi sätter loading till false, vilket betyder att vi inte längre är i ett laddningstillstånd.
      });
  }, []);

  if (loading) return <p>Loading...</p>;      // Om vi är i ett laddningstillstånd, returnerar vi en paragraf som säger "Loading...".

  return (
    <div>
      <h1>Game Sessions</h1>
      {sessions.length === 0 && <p>No game sessions found.</p>}     // Om loading är false och sessions-arrayen är tom, visar vi en paragraf som säger "No game sessions found.".
      {sessions.length > 0 && (     // Om loading är false och sessions-arrayen inte är tom, visar vi en lista med alla game sessions.
        <ul>
          {sessions.map((session) => (        // Vi mappar över sessions-arrayen och för varje session, returnerar vi en list-item som visar sessionens namn och status.
            <li key={session.id}>
              {session.name} - {session.status}   // Vi visar sessionens namn och status i list-item.
            </li>
          ))}
        </ul>
      )}
    </div>
  );


}
