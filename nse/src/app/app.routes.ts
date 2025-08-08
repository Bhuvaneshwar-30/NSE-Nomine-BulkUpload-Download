import { Routes } from '@angular/router';

export const routes: Routes = [
    {
    path:'',
    pathMatch: 'full',
       loadComponent: () => {
        return import('./components/bulk-upload/bulk-upload').then(m => m.BulkUploadComponent);
    },
},

{
    path: 'bulk-download',
    pathMatch: 'full',
    loadComponent:() =>{
        return import('./bulk-download/bulk-download').then(m => m.BulkDownloadComponent);
    },
}];
