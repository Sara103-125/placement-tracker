import { Routes } from '@angular/router';
import { ApplicationList } from './pages/application-list/application-list';

export const routes: Routes = [
  { path: '', redirectTo: 'applications', pathMatch: 'full' },
  { path: 'applications', component: ApplicationList },
  { path: '**', redirectTo: 'applications' },
];
