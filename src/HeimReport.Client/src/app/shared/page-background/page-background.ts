import { Component } from '@angular/core';

@Component({
  selector: 'app-page-background',
  standalone: true,
  template: `
    <main class="relative ml-24 mt-16 p-8 w-full min-h-screen bg-[#E7ECF3] overflow-hidden">
      <div class="absolute top-[-10%] left-[20%] h-125 w-125 rounded-full bg-blue-400/10 blur-[120px] pointer-events-none"></div>
      <div class="absolute bottom-[-10%] right-[-5%] h-150 w-150 rounded-full bg-indigo-300/15 blur-[140px] pointer-events-none"></div>

      <div class="relative z-10">
        <ng-content></ng-content>
      </div>
    </main>
  `
})
export class PageBackgroundComponent {}