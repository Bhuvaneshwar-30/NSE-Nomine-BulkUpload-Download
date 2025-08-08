import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "./components/header/header";
import { MatDialogModule} from '@angular/material/dialog';


@Component({
  selector: 'app-root',
  standalone:true,
  imports: [RouterOutlet, HeaderComponent, MatDialogModule],
  template: `<app-header></app-header>
<router-outlet></router-outlet>`,
  styleUrls: ['./app.css']
})
export class App {
}
