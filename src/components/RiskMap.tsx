import { useEffect, useMemo, useRef, useState } from 'react';

import type { TerritoryDto } from '@/api/generated/models';

const svgURL = "/risk-map.svg";
const  interactiveLayerID = "map-interactive-layer";
const territoryClass = "territory";

function nameToSvgId(name: string) {
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
}

export default function RiskMap({ territories = []}: RiskMapProps) {

    const [showText, setShowText] = useState(true);
  const [svgMarkup, setSvgMarkup] = useState<string | null>(null);
  const [hoveredId, setHoveredId] = useState<string | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const territoryBySvgId = useMemo(() => {
    const map = new Map<string, TerritoryDto>();
    territories.forEach((t) => map.set(nameToSvgId(t.name), t));
    return map;
  }, [territories]);
    
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
}
