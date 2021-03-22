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
    followers = [];
    following = [];
    recommended = [];

    followerCount = 0;
    followingCount = 0;

    constructor() {
        let storageData = JSON.parse(localStorage.getItem('auth'));
        if (storageData != undefined) {
            this.id = storageData.id;
            this.token = storageData.token;

            this.load();
            this.logged = true;
            this.loadEntries();
        }

        this._getRecommended();
    }

    set(data, load = true) {
        localStorage.setItem('auth', JSON.stringify(data));
        this.id = data.id;
        this.token = data.token;

        this.load();
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
        this.retweets = [];
        this.followers = [];
        this.following = [];
        this.followingCount = 0;
        this.followerCount = 0;
        localStorage.removeItem('auth');

        this._getRecommended();
    }

    async load() {
        const response = await fetch(`http://localhost:5000/api/users/${this.id}`);
        const data = await response.json();

        this._setData(data);
    }

    async loadEntries() {
        let response = await fetch(`http://localhost:5000/api/users/${this.id}/entries`);
        this.entries = await response.json();
        this.entries.reverse();

        response = await fetch(`http://localhost:5000/api/users/${this.id}/favorites`);
        this.favorites = await response.json();

        this.retweets = this.entries.filter(x => x.authorId != this.id);

        response = await fetch(`http://localhost:5000/api/users/${this.id}/followers`);
        this.followers = await response.json();

        response = await fetch(`http://localhost:5000/api/users/${this.id}/following`);
        this.following = await response.json();

        this._getRecommended();
    }

    follows(id) {
        return this.following.some(x => x.id == id);
    }

    async follow(user) {
        const response = await fetch(`http://localhost:5000/api/users/${user.id}/follow`, {
            method: 'POST',
            cache: 'no-cache',
            credentials: 'omit',
            redirect: 'follow',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + this.token
            }
        });

        if (response.status == 200) {
            this.following.push(user);
            this.followingCount++;
        }
    }

    async unfollow(user) {
        const response = await fetch(`http://localhost:5000/api/users/${user.id}/unfollow`, {
            method: 'DELETE',
            cache: 'no-cache',
            credentials: 'omit',
            redirect: 'follow',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + this.token
            }
        });

        if (response.status == 200) {
            let index = this.following.indexOf(user.id);
            this.following.splice(index, 1);
            this.followingCount--;
        }
    }

    async _getRecommended() {
        let id = 0;
        if (this.logged) {
            id = this.id;
        }

        const response = await fetch(`http://localhost:5000/api/users/recommended`, {
            method: 'POST',
            cache: 'no-cache',
            credentials: 'omit',
            redirect: 'follow',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(`${3} ${id}`) //request format: 'count userId'
        });
        this.recommended = await response.json();
        this.recommended.reverse();
    }

    _setData(data) {
        this.name = data.name;
        this.username = data.username;
        this.about = data.about;
        this.followerCount = data.followerCount;
        this.followingCount = data.followingCount;
    }
}

var user = new User();