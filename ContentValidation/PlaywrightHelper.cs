using Microsoft.Playwright;
public static class PlaywrightHelper
{
    public static async Task<IPage> GotoageWithRetriesAsync(IPage page, string url, int retryCount = 3)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 30000 });

                var response = await page.WaitForResponseAsync("**/*");

                if (!response.Ok)
                {
                    throw new Exception($"Page could not be opened for URL: {page.Url}");
                }

                return page;
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"Navigating to {url} (attempt {i + 1}/{retryCount})");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"An error occurred while navigating to {url}: {ex.Message}");
                throw;
            }
        }
        throw new InvalidOperationException("Something wrong in the rules.");
    }
}
