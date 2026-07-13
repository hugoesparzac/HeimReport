import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [],
  templateUrl: './button.html',
  styleUrl: './button.css',
})
export class ButtonComponent {
  variant = input<'primary' | 'secondary' | 'outline-dark'>('primary');

  type = input<'button' | 'submit'>('button');
  
  disabled = input<boolean>(false);
  
  fullWidth = input<boolean>(false); 
  
  clicked = output<void>();

  onClick() {
    if (!this.disabled()) {
      this.clicked.emit();
    }
  }
}
