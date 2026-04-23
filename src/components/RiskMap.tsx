import { useEffect, useMemo, useRef, useState } from 'react';


const svgURL = "/risk-map.svg";
const  interactiveLayerID = "map-interactive-layer";
const territoryClass = "territory";

function nameToSvgId(name: string) {
    return name.toLowerCase().replace(/\s+/g, "_");
}