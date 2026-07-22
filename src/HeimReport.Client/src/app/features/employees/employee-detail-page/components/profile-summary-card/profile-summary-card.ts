import { Component } from '@angular/core';
import { ButtonComponent } from '../../../../../shared/components/button/button';

@Component({
  selector: 'app-profile-summary-card',
  standalone: true,
  imports: [ButtonComponent],
  templateUrl: './profile-summary-card.html',
   host: {
    class: 'block h-full',
  },
})
export class ProfileSummaryCardComponent {
  
}