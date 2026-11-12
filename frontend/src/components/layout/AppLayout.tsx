import { NavLink, Outlet } from 'react-router';
import { DEMO_MODE } from '../../config';
import { DemoFooter } from './DemoFooter';
import { WakeUpBanner } from './WakeUpBanner';

const navItems = [
  { to: '/', label: 'Dashboard', end: true },
  { to: '/transactions', label: 'Transactions', end: false },
  { to: '/categories', label: 'Categories', end: false },
];

export function AppLayout() {
  return (
    <div className="flex min-h-screen flex-col bg-base-200">
      <header className="navbar bg-base-100 shadow-sm">
        <div className="mx-auto flex w-full max-w-6xl flex-wrap items-center gap-2 px-4">
          <NavLink to="/" className="flex items-center gap-2 text-xl font-bold">
            <img src="/favicon.svg" alt="" className="h-7 w-7" />
            CashStack
          </NavLink>
          <nav aria-label="Main" className="ml-auto">
            <ul className="menu menu-horizontal gap-1 px-0">
              {navItems.map((item) => (
                <li key={item.to}>
                  <NavLink
                    to={item.to}
                    end={item.end}
                    className={({ isActive }) => (isActive ? 'menu-active' : undefined)}
                  >
                    {item.label}
                  </NavLink>
                </li>
              ))}
            </ul>
          </nav>
        </div>
      </header>
      <WakeUpBanner />
      <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-6">
        <Outlet />
      </main>
      {DEMO_MODE && <DemoFooter />}
    </div>
  );
}
