import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

import { FinanceRoutingModule } from './finance-routing.module';

@NgModule({
  declarations: [],
  imports: [CommonModule, FinanceRoutingModule],
  providers: [provideHttpClient(withInterceptorsFromDi())],
})
export class FinanceModule {}
