import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JourneyFlow } from './journey-flow';

describe('JourneyFlow', () => {
  let component: JourneyFlow;
  let fixture: ComponentFixture<JourneyFlow>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JourneyFlow],
    }).compileComponents();

    fixture = TestBed.createComponent(JourneyFlow);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
