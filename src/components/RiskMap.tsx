import { useEffect, useMemo, useRef, useState } from 'react';

import type { TerritoryDto } from '@/api/generated/models';

const svgURL = "/risk-map.svg";
const  interactiveLayerID = "map-interactive-layer";
const territoryClass = "territory";
const selectedClass = "selected";
const adjacentClass = "adjacent";

export function nameToSvgId(name: string) {
    return name.toLowerCase().replace(/\s+/g, "_");
}

function prettifyId(id: string) {
    return id.split("_").map((part) => part.charAt(0).toUpperCase() + part.slice(1)).join(" ");
}

function buildInteractiveSvg(rawSvg: string): string {
  const doc = new DOMParser().parseFromString(rawSvg, "image/svg+xml");

  doc.querySelectorAll("[filter]").forEach((element) => {
    const ref = element.getAttribute("filter");
    if (ref === "url(#filter_texture)" || ref === "url(#filter_glow)") {
      element.removeAttribute("filter");
    }
  });

  const mapInDefs = doc.querySelector<SVGGElement>("defs > g#map");
  if (mapInDefs) {
    const layer = mapInDefs.cloneNode(true) as SVGGElement;
    layer.setAttribute("id", interactiveLayerID);
    layer.removeAttribute("fill");
    layer.querySelectorAll("path").forEach((path) => {
      path.setAttribute("fill", "transparent");
      path.setAttribute("stroke", "transparent");
      path.setAttribute("pointer-events", "all");
      path.classList.add(territoryClass);
    });
    doc.querySelector("svg > g[transform]")?.appendChild(layer);
  }

  return new XMLSerializer().serializeToString(doc.documentElement);
}

type RiskMapProps = {
    territories: TerritoryDto[];
    selectedSvgId?: string | null;
    onSelectChange?: (svgId: string | null) => void;
};

export default function RiskMap({
  territories = [],
  selectedSvgId = null,
  onSelectChange,
}: RiskMapProps)  {

    const [showText, setShowText] = useState(true);
  const [svgMarkup, setSvgMarkup] = useState<string | null>(null);
  const [hoveredId, setHoveredId] = useState<string | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const selectedRef = useRef(selectedSvgId);
  useEffect(() => {
    selectedRef.current = selectedSvgId;
  }, [selectedSvgId]);

  const territoryBySvgId = useMemo(() => {
    const map = new Map<string, TerritoryDto>();
    territories.forEach((t) => map.set(nameToSvgId(t.name), t));
    return map;
  }, [territories]);

  const territoryById = useMemo(() => {
    const map = new Map<number, TerritoryDto>();
    territories.forEach((t) => map.set(t.id, t));
    return map;
  }, [territories]);

   const adjacentSvgIds = useMemo(() => {
    if (!selectedSvgId) return new Set<string>();
    const selected = territoryBySvgId.get(selectedSvgId);
    if (!selected) return new Set<string>();
    const ids = new Set<string>();
    selected.adjacentTerritoryIds.forEach((id) => {
      const neighbor = territoryById.get(id);
      if (neighbor) ids.add(nameToSvgId(neighbor.name));
    });
    return ids;
  }, [selectedSvgId, territoryBySvgId, territoryById]);
    
  useEffect(() => {
    let cancelled = false;
    fetch(svgURL)
      .then((r) => r.text())
      .then((text) => {
        if (cancelled) return;
        const cleaned = text
          .replace(/<\?xml[^>]*\?>\s*/g, "")
          .replace(/<!DOCTYPE[^>]*>\s*/g, "");
        setSvgMarkup(buildInteractiveSvg(cleaned));
      });
    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    // Event delegation on the container instead of per-path listeners. React's
    // dangerouslySetInnerHTML may replace the inner SVG nodes without notifying
    // us, leaving per-path listeners attached to detached nodes. The container
    // div itself is React-owned and stable for the component's lifetime.
    const findTerritory = (target: EventTarget | null) =>
      target instanceof SVGPathElement &&
      target.classList.contains(territoryClass)
        ? target
        : null;

    const onOver = (e: MouseEvent) => {
      const t = findTerritory(e.target);
      if (t) setHoveredId(t.id);
    };
    const onOut = (e: MouseEvent) => {
      const t = findTerritory(e.target);
      if (t) {
        setHoveredId((current) => (current === t.id ? null : current));
      }
    };
    const onClick = (e: MouseEvent) => {
      const t = findTerritory(e.target);
      if (!t) return;
      const next = selectedRef.current === t.id ? null : t.id;
      onSelectChange?.(next);
    };

    container.addEventListener("mouseover", onOver);
    container.addEventListener("mouseout", onOut);
    container.addEventListener("click", onClick);
    return () => {
      container.removeEventListener("mouseover", onOver);
      container.removeEventListener("mouseout", onOut);
      container.removeEventListener("click", onClick);
    };
  }, [onSelectChange]);

  // Reflect the selected prop onto the DOM by toggling a class on the matching
  // path. Done imperatively because the SVG lives outside React's tree.
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;
    container
      .querySelectorAll(`.${territoryClass}.${selectedClass}`)
      .forEach((p) => p.classList.remove(selectedClass));
    if (selectedSvgId) {
      container
        .querySelector(`.${territoryClass}#${CSS.escape(selectedSvgId)}`)
        ?.classList.add(selectedClass);
    }
  }, [selectedSvgId, svgMarkup]);

  // Mark adjacent territories so the player can see which ones can be attacked
  // from the selected territory.
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;
    container
      .querySelectorAll(`.${territoryClass}.${adjacentClass}`)
      .forEach((p) => p.classList.remove(adjacentClass));
    adjacentSvgIds.forEach((id) => {
      container
        .querySelector(`.${territoryClass}#${CSS.escape(id)}`)
        ?.classList.add(adjacentClass);
    });
  }, [adjacentSvgIds, svgMarkup]);

  const hoveredTerritory = hoveredId ? territoryBySvgId.get(hoveredId) : null;

    return (
      <div className="relative w-full">
      <style>{`
        .risk-map svg { width: 100%; height: auto; display: block; }
        .risk-map.no-text svg text { display: none; }
        .risk-map .${territoryClass} {
          transition: stroke 120ms ease;
          cursor: pointer;
        }
        .risk-map .${territoryClass}.${adjacentClass} {
          stroke: #f97316 !important;
          stroke-width: 2.5 !important;
          stroke-opacity: 1 !important;
        }
        .risk-map .${territoryClass}.${selectedClass} {
          stroke: #ef4444 !important;
          stroke-width: 3 !important;
          stroke-opacity: 1 !important;
        }
        .risk-map .${territoryClass}:hover {
          stroke: #facc15 !important;
          stroke-width: 3 !important;
          stroke-opacity: 1 !important;
        }
      `}</style>

      <div className="mb-3 flex items-center gap-2 text-sm">
        <label className="inline-flex cursor-pointer items-center gap-2 text-slate-700">
          <input
            type="checkbox"
            checked={showText}
            onChange={(e) => setShowText(e.target.checked)}
            className="h-4 w-4 cursor-pointer"
          />
          Visa landstexter
        </label>
      </div>

      <div
        ref={containerRef}
        className={`risk-map ${showText ? "" : "no-text"}`}
        dangerouslySetInnerHTML={svgMarkup ? { __html: svgMarkup } : undefined}
      />
      {hoveredId && (
        <div className="absolute top-12 left-2 rounded bg-black/70 px-3 py-2 text-sm text-white pointer-events-none">
          <div className="font-semibold">
            {hoveredTerritory?.name ?? prettifyId(hoveredId)}
          </div>
          {hoveredTerritory ? (
            <div className="text-xs text-white/70">
              ID {hoveredTerritory.id} · Continent {hoveredTerritory.continentId}
            </div>
          ) : (
            <div className="text-xs text-amber-300">Ingen DB-matchning</div>
          )}
        </div>
      )}
    </div>

    );
}
