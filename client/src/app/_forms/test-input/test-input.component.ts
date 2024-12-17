import { NgIf } from '@angular/common';
import { Component, input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-test-input',
  imports: [NgIf, ReactiveFormsModule],
  templateUrl: './test-input.component.html',
  styleUrl: './test-input.component.css'
})
export class TestInputComponent implements ControlValueAccessor {
  label = input<string>('');
  type = input<string>('text');

  constructor(@Self() public ngControl: NgControl)
  {
    this.ngControl.valueAccessor = this
  }

  writeValue(obj: any): void {
  }

  registerOnChange(fn: any): void {
  }

  registerOnTouched(fn: any): void {
  }
  
  get control(): FormControl {
    return this.ngControl.control as FormControl
  }
}
