/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Base URL of the CashStack API, e.g. http://localhost:5198 */
  readonly VITE_API_URL?: string;
  /** "true" on the live demo (Render): demo footer and cold-start wording */
  readonly VITE_DEMO_MODE?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
