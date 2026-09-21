import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Money } from './money';

@Component({
  imports: [Money],
  template: '<app-money [amount]="amount" />',
})
class Host {
  amount = -1234.5;
}

describe('Money', () => {
  it('renders de-AT currency parts and preserves the negative sign', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();

    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('−€1\u00a0234,50');
    expect(fixture.nativeElement.querySelector('.cents')?.textContent).toBe(',50');
  });
});
