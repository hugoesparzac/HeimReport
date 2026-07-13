import { Component, input} from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [],
  templateUrl: './status-badge.html',
  styleUrl: './status-badge.css',
})
export class StatusBadgeComponent {
  label = input.required<string>();
  variant = input<'success' | 'warning' | 'danger' | 'info'>('info');
}
