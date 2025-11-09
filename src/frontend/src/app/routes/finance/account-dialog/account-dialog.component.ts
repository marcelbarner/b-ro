import { ChangeDetectionStrategy, Component, Inject, OnInit } from '@angular/core';

import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogRef,
  MatDialogModule,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { Account } from '@shared';

export interface AccountDialogData {
  account?: Account;
  mode: 'create' | 'edit';
}

@Component({
  selector: 'app-account-dialog',
  templateUrl: './account-dialog.component.html',
  styleUrl: './account-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
],
})
export class AccountDialogComponent implements OnInit {
  form: FormGroup;
  isEditMode: boolean;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AccountDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AccountDialogData
  ) {
    this.isEditMode = data.mode === 'edit';
    this.form = this.createForm();
  }

  ngOnInit(): void {
    if (this.isEditMode && this.data.account) {
      this.form.patchValue({
        name: this.data.account.name,
        iban: this.data.account.iban,
        currency: this.data.account.currency,
      });
    }
  }

  private createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      iban: [
        '',
        [
          Validators.required,
          Validators.pattern(/^[A-Z]{2}[0-9]{2}[A-Z0-9]+$/),
          Validators.maxLength(34),
        ],
      ],
      currency: [
        '',
        [Validators.required, Validators.pattern(/^[A-Z]{3}$/), Validators.maxLength(3)],
      ],
      initialBalance: [
        0,
        this.isEditMode ? [] : [Validators.required, Validators.min(0)],
      ],
    });
  }

  get title(): string {
    return this.isEditMode ? 'Edit Account' : 'Create Account';
  }

  get submitButtonText(): string {
    return this.isEditMode ? 'Update' : 'Create';
  }

  onSubmit(): void {
    if (this.form.valid) {
      const formValue = this.form.value;

      if (this.isEditMode) {
        // For edit, only send changed fields
        const updateData = {
          name: formValue.name,
          iban: formValue.iban,
          currency: formValue.currency,
        };
        this.dialogRef.close(updateData);
      } else {
        // For create, send all fields including initialBalance
        this.dialogRef.close(formValue);
      }
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
