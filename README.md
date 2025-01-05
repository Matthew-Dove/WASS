# WASS
Web Attached SStorage: a place to store all your stuff.  

## Intro

![Wass Overview](assets/images/wass-overview.png)

Like a NAS (*Network Attached Storage*), but with a web focus.  
The extra **S** in WAS**S**, is so the pronunciation is the same as NAS.  
WASS aims to be your backup, and access solution for your private files.  

## S3 Compatible Storage

WASS uses the AWS S3 SDK, and as such is compatible with any storage provider that supports the API.  
This is a bit of an industry standard, as such all the major players tend to support it (_to varying degrees_).   

## Compress

You can compress files before backing them up, WASS currently supports: `Gzip`, and `Brotli`.  
On restore you would instruct WASS to decompress the file.  

## Encrypt

You can encrypt files before backing them up, WASS currently supports: `AES` (_256-bit key with salt_).  
On restore you would instruct WASS to decrypt the file.  

## Tag

You can `tag` files once they have been backed up to a storage destination.  
Tags allow you to search for files containing said tag(_s_) later on.  
For example, you might tag a file by it's type (_mp4_), category (_video_), and characteristics (_funny_).  

## CLI API

WASS has a CLI for `backing up`, `restoring`, and `tagging` files.  
In general the commands follow this pattern: `> wass <verb> <file> [options]`.   

```console
> wass backup {file} [options]  
> wass restore {file} [options]
> wass tag {file} [options]
> wass help
```

Expanded `backup` example with options.  
```console
> wass backup --file=myfile.txt --compress=brotli --encrypt=aes --destination=s3
```

**Dash Style:**  
Use a single dash for short options (-f) and double dash for long options (--file).  
For parameters that expect a value, use the format `--key=value`, or `-k=value`.  

**Character Escape:**  
Escape special characters, and delimiters in option values; with a backslash `\`.  

**Commands:**
```
backup      Upload the specified file to the configured destination.
restore     Download the specified file from the configured destination.
tag         Add a tag to the file (multiple values separated with a colon ":").
help        Show help message, and exit.
```

**Options:**  
```
-cp,   --compress          Compress the file data before backing up: gzip | brotli.
-dp,   --decompress        Decompress the file data before restoring: gzip | brotli.
-en,   --encrypt           Encrypt file data before backing up: aes.
-de,   --decrypt           Decrypt file data before restoring: aes.
-dn,   --destination       Specify the backup destination found in the config (API must be S3 compatible).
-dr,   --dry-run           Simulate the process, with no side effects.
-nl,   --no-log            Disable logging for the run.
-tg,   --tags              Add tags to a backed up file.
```

**Config:**  
`Security.Password` is used as the key for file encryption.  
`Security.Salt` used for hashing operations.  
`Destination.Sources.*` is where you configure your backup sources, you can have as many as you like.  

All config values found in _appsettings.json_ can be overridden with environment variables.  
The convention is to follow the json path to the property you want to override, nested scopes are traversed with double underscores `__`.  
For example, to set a source's _SecretAccessKey_, the env var name would be: `Destination__Sources__S3__SecretAccessKey`.  
You would set `B2`'s destination in a similar way: `Destination__Sources__B2__SecretAccessKey`..  

```json
{
  "Security": {
    "Password": "password",
    "Salt": "salt"
  },
  "Destination": {
    "Sources": {
      "S3": {
        "AccessKeyId": "accessKeyId",
        "SecretAccessKey": "secretAccessKey",
        "Bucket": "bucketName",
        "Region": "us-east-2",
        "ServiceUrl": "https://s3.us-east-2.amazonaws.com/"
      },
      "B2": {
        "AccessKeyId": "applicationKeyId",
        "SecretAccessKey": "applicationKey",
        "Bucket": "bucketName",
        "Region": "us-west-004",
        "ServiceUrl": "https://s3.us-west-004.backblazeb2.com/"
      }
    }
  }
}
```

Credentials / keys / passwords, bucket names, and destinations (_etc_) are pulled from the local config.  
Exit codes are based on success, or failure:  
- `0` - Success.
- `1` - Error (_operation was not successful, or an internal exception occurred_).
- `2` - Bad Request (_invalid commands, or arguments_).

## Philosophy

WASS does not support file deletes.  
WASS does not support file updates.  

If you would like to remove something from storage, you must do so manually by going to your 3rd party storage console, or local drive interfaces.  
In the same vein, if you update a file to "empty", that's the same as deleting / removing it.  

Why? Because if you are backing up a file though WASS, it is important to you; and you do not want to lose it by deleting, or updating local files.  
We want to avoid replicating source file changes to backup target locations.  
By not implementing delete, or update functionity in WASS, it will be _harder_ for WASS to lose your data though any program bugs; or user actions.  
Ideally any api keys you provide WASS contain create-only semantics, as an extra layer of protection.  

WASS treats files as immuable objects, all changes are handled with additions, not overwrites (_a file is identified by it's hash_).  
WASS generally does not use immuable polices provided by 3rd party systems, as they are not fully consistent across the board.  
i.e. your local NAS does not have a data governance policy that WASS can enable with an API call.  
That said there is nothing stopping you from adding them yourself, WASS does not care as long as it still has read; and write permissions.  

When uploading a file though WASS, firstly we search for the hash at the target server.  
If it exists, we do not upload the file data; but we may still upload metadata (_such as the source path, or search tags_).  

WASS handles metadata the same way as files - immuable & create-only.  
For example when adding _tags_ for a file, we do not have a tags file that is updated over time.  
Instead new files are created under a path stemming from the file's hash value, containing the necessary data.  

As a side note, this philosophy makes WASS a poor choice for backing up things you are currently working on.  
If you are writing a story let's say, each time you saved it; WASS will treat it as a new file (_since the hash has changed_).  
Over time, you might have hundreds of files for your story (_or more!_).  
This gets worse when the work is something larger, for example a photoshop file; or a video that you're editing.  
Therefore, do not keep any working directories under a target location used by WASS.  

## TODO

* `Wass.Cli` - global dotnet tool to administer WASS though the console.
* `Wass.WebApp` - aws lambda to provide file access over the web, + [Progressive Web App](https://web.dev/add-manifest/) for moblie.
* `Wass.SelfHost` - shares functionality from __Wass.WebApp__, and __Wass.Cli__ to run WASS locally.

![wass-product-diagram](assets/images/wass-product-diagram.png)

## Object Storage

These services have S3 compatible APIs, offer competitive pricing, and are popular community choices:

* [AWS S3](https://aws.amazon.com/s3/)
* [BackBlaze B2](https://www.backblaze.com/cloud-storage)
* [CloudFlare R2](https://www.cloudflare.com/en-au/developer-platform/r2/)
* [Wasabi](https://wasabi.com/)
* [Hetzner Object Storage](https://docs.hetzner.com/storage/object-storage/)
* [DigitalOcean Spaces](https://www.digitalocean.com/products/spaces)
* [IDrive](https://www.idrive.com/s3-storage-e2/)
* [Petabox](https://petabox.io/)
* [Storj](https://www.storj.io/)
* [IceDrive](https://icedrive.net/)

## CDN

When viewing content from WASS (_online_), it's a good idea to use a CDN.  
WASS does not have any native support for CDNs, but using one on top is fine.  
Here are some popular choices:  

* [Bunny](https://bunny.net/cdn/)
* [Cloudflare](https://www.cloudflare.com/cdn/)
* [Fastly](https://www.fastly.com/products/cdn)
* [Keycdn](https://www.keycdn.com/)
* [Cloudfront](https://aws.amazon.com/cloudfront/)

Note that media (_video_) streaming though CDNs is typically sold as a separate / specialised product.  
WASS can pull directly from the source (_without a CDN_), but you may encounter bandwidth limitations; or get charged comical egress traffic fees.  

# Credits

* [Icon](https://www.flaticon.com/free-icon/bird_2630452) made by [Vitaly Gorbachev](https://www.flaticon.com/authors/vitaly-gorbachev) from [Flaticon](https://www.flaticon.com/).

# Changelog

## 0.0.1

* Upload empty project to github.