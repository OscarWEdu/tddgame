import { useEffect, useMemo, useRef, useState } from 'react';


const svgURL = "/risk-map.svg";
const  interactiveLayerID = "map-interactive-layer";
const territoryClass = "territory";

function nameToSvgId(name: string) {
    return name.toLowerCase().replace(/\s+/g, "_");
}

function prettifyId(id: string) {
    return id.split("_").map((part) => part.charAt(0).toUpperCase() + part.slice(1)).join(" ");
}