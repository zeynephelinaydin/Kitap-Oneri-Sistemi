using KitapOneri.Models;
using KitapOneri.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KitapOneri.Services
{
    public class RecommendationService
    {
        private readonly ApplicationDbContext _context;
        private static List<Book> _cachedBooks;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Initialize(List<Book> allBooks)
        {
            _cachedBooks = allBooks;
        }

        private async Task<List<Book>> GetAllBooksAsync()
        {
            if (_cachedBooks == null)
            {
                _cachedBooks = await _context.books
                    .Include(b => b.rating) // 📌 Rating bilgisi dahil edildi
                    .ToListAsync();
            }
            return _cachedBooks;
        }

        private double CosineSimilarity(string text1, string text2)
        {
            var vector1 = GetWordFrequency(text1);
            var vector2 = GetWordFrequency(text2);

            var allWords = vector1.Keys.Union(vector2.Keys);
            double dotProduct = 0, magnitude1 = 0, magnitude2 = 0;

            foreach (var word in allWords)
            {
                int freq1 = vector1.ContainsKey(word) ? vector1[word] : 0;
                int freq2 = vector2.ContainsKey(word) ? vector2[word] : 0;

                dotProduct += freq1 * freq2;
                magnitude1 += Math.Pow(freq1, 2);
                magnitude2 += Math.Pow(freq2, 2);
            }

            if (magnitude1 == 0 || magnitude2 == 0) return 0;
            return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
        }

        private Dictionary<string, int> GetWordFrequency(string text)
        {
            var frequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var words = Regex.Split(text ?? "", @"\W+");

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word)) continue;
                if (frequencies.ContainsKey(word))
                    frequencies[word]++;
                else
                    frequencies[word] = 1;
            }

            return frequencies;
        }

        private List<Book> GetTopSimilarBooks(Dictionary<Book, double> scoredBooks, int topN)
        {
            return scoredBooks
                .OrderByDescending(kv => kv.Value)
                .ThenBy(_ => Guid.NewGuid()) // rastgelelik için
                .Take(topN)
                .Select(kv => kv.Key)
                .ToList();
        }

        public async Task<List<Book>> GetSimilarBooks(string newExplanationRoot, string newNameRoot, int topN)
        {
            var allBooks = await GetAllBooksAsync();
            var scores = new Dictionary<Book, double>();

            foreach (var book in allBooks)
            {
                double sim = CosineSimilarity(book.new_explanation_root + " " + book.new_name_root, newExplanationRoot + " " + newNameRoot);
                if (sim > 0) scores[book] = sim;
            }

            return GetTopSimilarBooks(scores, topN);
        }

        public async Task<List<Book>> GetSimilarBooksByRootMatch(string newExplanationRoot, string newNameRoot, int topN)
        {
            var allBooks = await GetAllBooksAsync();
            var scores = new Dictionary<Book, double>();

            foreach (var book in allBooks)
            {
                double sim = CosineSimilarity(book.new_explanation_root + " " + book.new_name_root, newExplanationRoot + " " + newNameRoot);
                if (sim > 0) scores[book] = sim;
            }

            return GetTopSimilarBooks(scores, topN);
        }

        public async Task<List<Book>> GetBooksByNameSimilarity(string newNameRoot, int topN)
        {
            var allBooks = await GetAllBooksAsync();
            var scores = new Dictionary<Book, double>();

            foreach (var book in allBooks)
            {
                double sim = CosineSimilarity(book.new_name_root, newNameRoot);
                if (sim > 0) scores[book] = sim;
            }

            return GetTopSimilarBooks(scores, topN);
        }

        public async Task<List<Book>> GetBooksByAuthor(string author, string newNameRoot, int topN)
        {
            var allBooks = await GetAllBooksAsync();
            var scores = new Dictionary<Book, double>();

            foreach (var book in allBooks)
            {
                double sim = CosineSimilarity(book.author + " " + book.new_name_root, author + " " + newNameRoot);
                if (sim > 0) scores[book] = sim;
            }

            return GetTopSimilarBooks(scores, topN);
        }

        public async Task<List<Book>> GetBooksByGenre(string genre, string newNameRoot, int topN)
        {
            var allBooks = await GetAllBooksAsync();
            var scores = new Dictionary<Book, double>();

            foreach (var book in allBooks)
            {
                double sim = CosineSimilarity(book.genre + " " + book.new_name_root, genre + " " + newNameRoot);
                if (sim > 0) scores[book] = sim;
            }

            return GetTopSimilarBooks(scores, topN);
        }

        public async Task<List<Book>> GetHybridRecommendations(string newNameRoot, string newExplanationRoot, string explanation, string nameRoot, string author, string genre, int totalRecommendations)
        {
            var resultDict = new Dictionary<Book, double>();

            async Task AddScores(Task<List<Book>> booksTask, double weight)
            {
                var books = await booksTask;
                foreach (var book in books)
                {
                    if (resultDict.ContainsKey(book))
                        resultDict[book] += weight;
                    else
                        resultDict[book] = weight;
                }
            }

            await AddScores(GetSimilarBooksByRootMatch(newExplanationRoot, newNameRoot, 15), 1.5);
            await AddScores(GetSimilarBooks(explanation, nameRoot, 15), 1.3);
            await AddScores(GetBooksByNameSimilarity(newNameRoot, 10), 2.0);
            await AddScores(GetBooksByAuthor(author, nameRoot, 10), 1.8);
            await AddScores(GetBooksByGenre(genre, nameRoot, 10), 1.2);

            return GetTopSimilarBooks(resultDict, totalRecommendations);
        }
    }
}
