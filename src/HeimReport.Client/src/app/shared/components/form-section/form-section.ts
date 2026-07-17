import { Component, input } from '@angular/core';

@Component({
  selector: 'app-form-section',
  standalone: true,
  imports: [],
  templateUrl: './form-section.html',
   host: {
    class: 'block',
  },
})
export class FormSectionComponent {
  icon = input.required<string>();
  title = input.required<string>();
}