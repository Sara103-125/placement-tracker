import { JobApplication, STATUS_LABELS } from '../models/job-application';

/**
 * Builds a CSV file from the applications and makes the browser download it.
 * Everything happens in the browser: the data is already on screen, so no extra API call is needed.
 */
export function downloadApplicationsCsv(applications: JobApplication[]): void {
  const header = ['Company', 'Role', 'Location', 'Status', 'Date applied', 'Deadline', 'Job link', 'Notes'];
  const rows = applications.map((a) => [
    a.company,
    a.role,
    a.location ?? '',
    STATUS_LABELS[a.status],
    a.dateApplied ?? '',
    a.deadline ?? '',
    a.jobLink ?? '',
    a.notes ?? '',
  ]);

  const csv = [header, ...rows].map((row) => row.map(escapeCell).join(',')).join('\r\n');

  // "﻿" tells Excel the file is UTF-8, so accents and symbols show correctly.
  const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = `placement-applications-${new Date().toISOString().slice(0, 10)}.csv`;
  link.click();
  URL.revokeObjectURL(url);
}

/**
 * CSV rules: wrap a value in quotes if it contains a comma, quote or new line, and double any quotes.
 * Values starting with = + - @ are prefixed with ' so spreadsheet apps don't run them as formulas.
 */
function escapeCell(value: string): string {
  let text = /^[=+\-@]/.test(value) ? `'${value}` : value;
  if (/[",\r\n]/.test(text)) {
    text = `"${text.replace(/"/g, '""')}"`;
  }
  return text;
}
