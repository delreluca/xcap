# Market capitalisation cross section

## What is it for

This tool shows market-neutral weights across different regions and company sizes. MSCI publishes fact sheets of different indices containing their total market capitalization. This tool shows the cross-section of developed marktes, emerging markets and _Small Cap_, _Standard_ (mid to large), _IMI_ (small to large) and _All Cap_ (_IMI_ plus micro-cap).

## Disclaimer

I am in no way affiliated with MSCI. Using this tool instead of manually downloading PDFs saves me time, but if you intend to use this in an automated or obsessive way, you might violate terms of services. Use at your own risk.

The distinction between developed and emerging markets and between different sizes is up to the index provider and not consistent between indices of different providers. For example FTSE and MSCI indices differ in their classification of Korea as well as their small cap threshold.

This tool parses the fact sheet PDF text extraction and may thus break without notice at anytime. Use at your own risk.

## Usage

Needs .NET 5. Use the `dotnet` CLI tool to build and run.

```sh
cd xcap
# The following call will show weights with ACWI IMI = 100%
dotnet run
# You can change which index represents 100% like this
dotnet run ACWI "All Cap"
```
