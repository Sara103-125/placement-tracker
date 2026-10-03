import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/** The public front page: what the app is, what it does, and buttons to sign up or log in. */
@Component({
  selector: 'app-landing',
  imports: [RouterLink],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class Landing {
  protected readonly features = [
    {
      title: 'Kanban board',
      text: 'Drag applications between stages, from Wishlist to Offer. Every move is saved instantly.',
      icon: 'M3 3h4v10H3zM9 3h4v6H9z',
    },
    {
      title: 'Dashboard & funnel',
      text: 'See where everything stands, and how many applications reach interview and offer.',
      icon: 'M3 13V8M7 13V4M11 13V9M14 13H2',
    },
    {
      title: 'Deadline alerts',
      text: 'Overdue and due-soon deadlines are flagged in the table, on the board and on the dashboard.',
      icon: 'M8 5v3.5l2 1.5M8 14A6 6 0 1 0 8 2a6 6 0 0 0 0 12Z',
    },
    {
      title: 'Interview scheduler',
      text: 'Add online tests, interviews and calls to each application, and see what is coming this week.',
      icon: 'M2.5 3.5h11v10h-11zM2.5 6.5h11M5.5 2v3M10.5 2v3',
    },
    {
      title: 'Status history',
      text: 'Every status change is recorded with a date, shown as a timeline for each application.',
      icon: 'M8 2v12M8 4h4M8 8h3M8 12h5M4 4h0M4 8h0M4 12h0',
    },
    {
      title: 'Export to CSV',
      text: 'Download your applications as a spreadsheet whenever you need them.',
      icon: 'M8 2v8M5 7l3 3 3-3M3 13h10',
    },
  ];

  protected readonly techStack = ['Angular', 'ASP.NET Core 8', 'Entity Framework Core', 'SQL Server', 'JWT authentication', 'xUnit'];
}
