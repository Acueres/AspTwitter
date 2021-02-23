class Entries {
    all = null;
    userEntries = null;

    constructor() {
        this.loadEntries();
    }

    async loadEntries() {
        const response = await fetch(`http://localhost:5000/api/entries`);
        const data = await response.json();
        this.all = data;
        this.all.reverse();
    }

    async loadUserEntries(id) {
        const response = await fetch(`http://localhost:5000/api/users/${id}/entries`);
        const data = await response.json();
        this.userEntries = data;
        this.userEntries.reverse();
    }

    add(entry) {
        this.all.splice(0, 0, entry);
        this.userEntries.splice(0, 0, entry);
    }

    deleteFromProfile(index, id) {
        this.userEntries.splice(index, 1);

        index = this.all.findIndex((e) => e.id == id);

        if (index != -1) {
            this.all.splice(index, 1);
        }
    }

    deleteFromHome(index, id) {
        this.all.splice(index, 1);

        index = this.userEntries.findIndex((e) => e.id == id);

        if (index != -1) {
            this.userEntries.splice(index, 1);
        }
    }
}

var entries = new Entries();