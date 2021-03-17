class User {
    logged = false;
    id = null;
    name = null;
    username = null;
    about = null;
    token = null;
    entries = [];
    favorites = [];
    retweets = [];

    constructor() {
        let storageData = JSON.parse(localStorage.getItem('user'));
        if (storageData != undefined) {
            this._setData(storageData);
            this.logged = true;
            this.loadEntries();
        }
    }

    set(data, load = true) {
        localStorage.setItem('user', JSON.stringify(data));
        this._setData(data);
        this.logged = true;

        if (load) {
            this.loadEntries();
        }
    }

    addEntry(entry) {
        this.entries.splice(0, 0, entry);
    }

    deleteEntry(id) {
        let index = this.entries.findIndex(x => x.id == id);
        if (index != -1) {
            this.entries.splice(index, 1);
        }
    }

    update() {
        localStorage.setItem('user', JSON.stringify({
            id: this.id,
            name: this.name,
            username: this.username,
            about: this.about,
            token: this.token
        }));
    }

    clear() {
        this.logged = false;
        this.id = null;
        this.name = null;
        this.username = null;
        this.about = null;
        this.token = null;
        this.favorites = [];
        localStorage.removeItem('user');
    }

    async loadEntries() {
        let response = await fetch(`http://localhost:5000/api/users/${this.id}/entries`);
        this.entries = await response.json();
        this.entries.reverse();

        response = await fetch(`http://localhost:5000/api/users/${this.id}/favorites`);
        this.favorites = await response.json();

        this.retweets = this.entries.filter(x => x.authorId != this.id);
    }

    _setData(data) {
        this.id = data.id;
        this.name = data.name;
        this.username = data.username;
        this.about = data.about;
        this.token = data.token;
    }
}

var user = new User();