/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Base URL of the CashStack API, e.g. http://localhost:5198 */
  readonly VITE_API_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
