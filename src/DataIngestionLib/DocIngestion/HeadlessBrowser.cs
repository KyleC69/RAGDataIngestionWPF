// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         HeadlessBrowser.cs
// Author: Kyle L. Crowder
// Build Num: 140751



using System.IO;
using System.Runtime.CompilerServices;

using PuppeteerSharp;
using PuppeteerSharp.Media;




namespace DataIngestionLib.DocIngestion;





public interface IHeadlessBrowser
{

    /// <summary>
    ///     Navigates to the specified URL and returns the page source.
    ///     Resources are disposed after retrieval.
    /// </summary>
    /// <param name="url">Fully qualified URL to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTML source of the page.</returns>
    /// <exception cref="ArgumentException">Thrown when the URL is null, empty, or invalid.</exception>
    Task<string> GetPageSourceAsync(string url, CancellationToken cancellationToken = default);
}





public class HeadlessBrowser : IDisposable, IHeadlessBrowser
{

    /// <inheritdoc />
    public void Dispose()
    {
        // No unmanaged resources to dispose, but method is implemented to satisfy IDisposable interface.
    }








    /// <summary>
    ///     Navigates to the specified URL and returns the page source.
    ///     Resources are disposed after retrieval.
    /// </summary>
    /// <param name="url">Fully qualified URL to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTML source of the page.</returns>
    /// <exception cref="ArgumentException">Thrown when the URL is null, empty, or invalid.</exception>
    public async Task<string> GetPageSourceAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL must not be null or empty.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new ArgumentException("URL must be an absolute URI.", nameof(url));
        }

        IBrowser browser = await StartAsync().ConfigureAwait(false);
        await using ConfiguredAsyncDisposable browser1 = browser.ConfigureAwait(false);
        IPage page = await browser.NewPageAsync().ConfigureAwait(false);
        await using ConfiguredAsyncDisposable page1 = page.ConfigureAwait(false);

        NavigationOptions navigationOptions = new() { WaitUntil = [WaitUntilNavigation.Networkidle0] };

        IResponse unused = await page.GoToAsync(url, navigationOptions).ConfigureAwait(false);
        return await page.GetContentAsync().ConfigureAwait(false);
    }








    public async Task CaptureScreenshotAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL must not be null or empty.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new ArgumentException("URL must be an absolute URI.", nameof(url));
        }

        IBrowser browser = await StartAsync().ConfigureAwait(false);
        await using ConfiguredAsyncDisposable browser1 = browser.ConfigureAwait(false);
        IPage page = await browser.NewPageAsync().ConfigureAwait(false);
        await using ConfiguredAsyncDisposable page1 = page.ConfigureAwait(false);
        NavigationOptions navigationOptions = new() { WaitUntil = [WaitUntilNavigation.Networkidle0] };
        IResponse unused = await page.GoToAsync(url, navigationOptions).ConfigureAwait(false);

        await page.EmulateMediaTypeAsync(MediaType.Screen).ConfigureAwait(false);
        await page.PdfAsync(Path.Combine("E:\\capturedpages", DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".pdf")).ConfigureAwait(false);

    }








    /// <summary>
    ///     Launches a headless Chromium instance using PuppeteerSharp.
    /// </summary>
    /// <returns>Initialized <see cref="IBrowser" /> instance.</returns>
    private static async Task<IBrowser> StartAsync()
    {


        LaunchOptions launchOptions = new()
        {
                AcceptInsecureCerts = false,
                Headless = false,
                HeadlessMode = HeadlessMode.False,
                Args = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-gpu"],
                Timeout = 300000,
                DumpIO = false,
                Devtools = false,
                IgnoreDefaultArgs = false,
                EnqueueTransportMessages = false,
                Browser = SupportedBrowser.Chrome,
                EnqueueAsyncMessages = false,
                WaitForInitialPage = false
        };

        return await Puppeteer.LaunchAsync(launchOptions).ConfigureAwait(false);
    }
}