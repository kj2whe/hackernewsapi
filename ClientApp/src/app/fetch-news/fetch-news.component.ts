import { Component, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { FormControl, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-fetch-news',
  templateUrl: './fetch-news.component.html'
})
export class FetchNewsComponent {
  public newsList: NewsList[];
  public _baseUrl: string;
  public _http: HttpClient;
  public selectedOption: string;
  public pageSize: string;
  public SearchText: string;
  public selectionForm: FormGroup;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this._http = http;
    this._baseUrl = baseUrl;
    this.pageSize = '20';

    this.getStories('', this.pageSize);
  }

  getStoriesClick(): void {
    this.newsList = null;

    let size = this.pageSize;

    if (!this.pageSize) {
      size = '5';
    }

    if (!this.SearchText) {
      size = '';
    }

    this.getStories(this.SearchText, size);
  }

  getStories(textToSearchFor: string, pageSize: string): void {
    const params = new HttpParams()
      .set('textToSearchFor', textToSearchFor)
      .set('numberOfStoriesToReturn', pageSize);

    this._http.get<NewsList[]>(this._baseUrl + 'news', { params: params }).subscribe(result => {
      this.newsList = result;
    }, error => console.error(error));  
  }
}

interface NewsList {
  link: string;
  title: string;
}
