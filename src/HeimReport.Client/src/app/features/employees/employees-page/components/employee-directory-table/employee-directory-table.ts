import { Component } from '@angular/core';
import { ButtonComponent } from '../../../../../shared/components/button/button';
import { StatusBadgeComponent } from '../../../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-employee-directory-table',
  standalone: true,
  imports: [ButtonComponent, StatusBadgeComponent],
  templateUrl: './employee-directory-table.html',
})
export class EmployeeDirectoryTableComponent {}