import { Routes } from '@angular/router';
import { ApplicationForm } from './pages/application-form/application-form';
import { ApplicationList } from './pages/application-list/application-list';
import { AuthPage } from './pages/auth/auth-page';
import { Board } from './pages/board/board';
import { Dashboard } from './pages/dashboard/dashboard';
import { Landing } from './pages/landing/landing';
import { authGuard, guestGuard } from './services/auth.guards';

export const routes: Routes = [
  // Public pages (logged-in users are sent to the dashboard instead)
  { path: '', component: Landing, pathMatch: 'full', canActivate: [guestGuard] },
  { path: 'login', component: AuthPage, data: { mode: 'login' }, canActivate: [guestGuard] },
  { path: 'register', component: AuthPage, data: { mode: 'register' }, canActivate: [guestGuard] },

  // Pages that need an account
  { path: 'dashboard', component: Dashboard, canActivate: [authGuard] },
  { path: 'applications', component: ApplicationList, canActivate: [authGuard] },
  { path: 'applications/new', component: ApplicationForm, canActivate: [authGuard] },
  { path: 'applications/:id/edit', component: ApplicationForm, canActivate: [authGuard] },
  { path: 'board', component: Board, canActivate: [authGuard] },

  { path: '**', redirectTo: '' },
];
