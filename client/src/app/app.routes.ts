import { Routes } from '@angular/router';
import { ApplicationForm } from './pages/application-form/application-form';
import { ApplicationList } from './pages/application-list/application-list';
import { Board } from './pages/board/board';
import { Dashboard } from './pages/dashboard/dashboard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: Dashboard },
  { path: 'applications', component: ApplicationList },
  { path: 'applications/new', component: ApplicationForm },
  { path: 'applications/:id/edit', component: ApplicationForm },
  { path: 'board', component: Board },
  { path: '**', redirectTo: 'dashboard' },
];
