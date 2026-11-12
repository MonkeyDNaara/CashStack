import { REPO_URL } from '../../config';

/** Only on the live demo (VITE_DEMO_MODE): visitors share one database without login */
export function DemoFooter() {
  return (
    <footer className="border-t border-base-300 py-3 text-center text-sm text-base-content/60">
      Live demo · data is shared with other visitors and may be reset ·{' '}
      <a href={REPO_URL} className="link link-primary" target="_blank" rel="noreferrer">
        Code on GitHub
      </a>
    </footer>
  );
}
