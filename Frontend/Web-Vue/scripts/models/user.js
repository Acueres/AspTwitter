class User {
    id = null;
    name = null;
    username = null;
    about = null;

    entries = [];
    favorites = [];
    retweets = [];
    followers = [];
    following = [];

    followerCount = 0;
    followingCount = 0;

    constructor(id) {
        this.id = id;
    }

    async load() {
        if (this.id == null) {
            return;
        }

        const response = await fetch(server + `api/users/${this.id}`);
        const data = await response.json();

        this._setData(data);

        await this.loadEntries();
    }

    async loadEntries() {
        if (this.id == null) {
            return;
        }

        let response = await fetch(server + `api/users/${this.id}/entries`);
        this.entries = await response.json();
        this.entries.reverse();

        response = await fetch(server + `api/users/${this.id}/favorites`);
        this.favorites = await response.json();

        response = await fetch(server + `api/users/${this.id}/retweets`);
        this.retweets = await response.json();
        this.retweets.reverse();

        response = await fetch(server + `api/users/${this.id}/followers`);
        this.followers = await response.json();

        response = await fetch(server + `api/users/${this.id}/following`);
        this.following = await response.json();
    }

    _setData(data) {
        this.name = data.name;
        this.username = data.username;
        this.about = data.about;
        this.followerCount = data.followerCount;
        this.followingCount = data.followingCount;
    }
}