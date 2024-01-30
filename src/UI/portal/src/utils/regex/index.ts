export class RegexHelper {

    private static digitRegex = /\d+/;

    public static getNumberByText(title?: string | null): number {
        if (!title) return Number.MAX_VALUE;
        const match = this.digitRegex.exec(title);
        return match ? parseInt(match[0]) : Number.MAX_VALUE;
    }
}