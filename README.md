# WASS
Web Attached SStorage: a place to store all your stuff.  

![Wass Overview](assets/images/wass-overview.png)

## Intro

Like a NAS (*Network Attached Storage*), but with a web focus.  
The extra **S** in WAS**S**, is so the pronunciation is the same as NAS.  
WASS aims to be your backup for your private files.  

## S3 Compatible Storage

WASS uses the AWS S3 SDK, and as such is compatible with any storage provider that supports the API.  
This is an industry standard, as such all the major players tend to support it (_to varying degrees_).   

## Backup

Select a local file, and upload it to a specified (_S3 compatible_) destination.  
You may backup the same file to many destinations, across multiple commands.  
Backup is **not** a sync. It is write once - only. Backup will not delete, and will not update existing files.  
Backup enables governance protection where possible to keep your files safe.  

## Restore

Select a destination file, and download it locally (_based off the config's "Download.LocalRootPath"_).  
If the file was compressed, or encrypted; WASS will revert the file to its original form before writing to disk.  
i.e. the file will be stored locally in "plaintext".  

## Compress

You can compress files before backing them up, WASS currently supports: `Gzip` (_faster_), and `Brotli` (_better compression size_).  
On restore WASS will decompress the file.  

## Encrypt

You can encrypt files before backing them up, WASS currently supports: `AES` (_256-bit key with salt_).  
On restore WASS will decrypt the file.  

## Tag

You can `tag` files once they have been backed up to a storage destination.  
Tag allows you to search for files linked to said tag(_s_) later on.  
For example, you might tag a file by it's type (_mp4_), category (_video_), and characteristics (_funny_).  
Seperate tag values in a single command are delimited by a colon (`:`), i.e. "_mp4:video:funny_".

## CLI API

WASS has a CLI for `backing up`, `restoring`, and `tagging` files.  
In general the commands follow this pattern: `> wass <verb> <file> [options]`.   

```console
> wass help
> wass backup {file} [options]  
> wass restore {filehash} [options]
> wass tag {file}|{filehash} [options]
> wass encryption {file} [options]
> wass decryption {file} [options]
> wass compression {file} [options]
> wass decompression {file} [options]
> wass salt [options]
> wass password [options]
```

**Dash Style:**  
Use a single dash for short options (-f) and double dash for long options (--file).  
For parameters that expect a value, use the format `--key=value`, or `-k=value`.  

**Character Escape:**  
Escape special characters, and delimiters in option values; with a backslash `\`.  

**Commands:**
```console
backup          Upload the specified file to the configured destination.
restore         Download the specified file(hash) from the configured destination.
help            Show help message, and exit.
tag             Add tags to the file at a specified destination (colon delimited - "tag1:tag2").
encryption      Encrypt a file, and store the result locally.
decryption      Decrypt a file, and store the result locally.
compression     Compress a file, and store the result locally.
decompression   Decompress a file, and store the result locally.
salt            Generate a cryptographic salt of the specified size in bytes, presented in hex.
password        Generate a cryptographic password of the specified character length, using a-z, A-Z, 0-9, and special characters.
```

**Options:**  
```console
-cp,   --compress          The compression algorithm to use when compressing, or decompressing: gzip | brotli.
-en,   --encrypt           The encryption algorithm to use when encrypting, or decrypting: aes.
-dn,   --destination       Specify the backup destination found in the config (API must be S3 compatible).
-dr,   --dry-run           Simulate the process, with no side effects.
-nl,   --no-log            Disable logging for the run.
-tg,   --tags              Add tags to a backed up file.
-sz,   --size              Specify the size of the salt, or password to generate.
-fh,   --file-hash         The lowercase hex string of the file's hash to restore.
-nt,   --no-template       Will not use a template when creating config files, prevents reusing redundant data.
-ns,   --no-schema         Won't create WASS metadata objects under the root ~/wass/* directory when restoring a file.
-ps,   --print-secret      By default sensitive values such as salts, and passwords are sent to the clipboard; this flag sends them to stdout: "SECRET#{VALUE}".
```

**Examples:**  
```console
wass help

wass backup myfile.txt --destination=s3
wass backup myfile.txt --destination=s3 --compress=brotli --encrypt=aes --dry-run --no-log

wass restore myfile.txt --destination=s3

wass tag myfile.txt --tags="tag"
wass tag myfile.txt --tags="tag1:tag2:tag3" --encrypt=aes

wass compression myfile.txt --compress=brotli
wass decompression myfile.txt.wass.brotli --compress=brotli

wass encryption myfile.txt --encrypt=aes
wass decryption myfile.txt.wass.aes --encrypt=aes

wass salt --size=16
wass salt --size=16 --print-secret

wass password --size=20
wass password --size=20 --print-secret
```

**Exit Codes:**
* `0` Success.
* `1` Error (_operation was not successful, or an exception occurred_).
* `2` Bad Request (_invalid cli commands, or arguments_).

**Config:**  
`Security.Password` is used as the key for file encryption (_PasswordKeyId is sent in plaintext metadata, value is not sensitive_).  
`Security.Salt` used for hashing operations (_SaltKeyId is sent in plaintext metadata, value is not sensitive_).  
`Destination.Sources.*` is where you configure your backup sources, you can have as many as you like.  
`Download.LocalRootPath` is the path to restore downloaded files to.  
`Cli.Options` common args you'd like appended to every call (_i.e. --no-log_). Cannot be used to set config values though command line args.  

Config values found in _appsettings.json_ | _appsettings.{stage}.json_, can be overridden with environment variables, and command line args.  

**Environment Variables:**  
The convention is to follow the json path to the property you want to override, nested scopes are traversed with double underscores `__`.  
For example, to set a source's _SecretAccessKey_, the env var name would be: `Destination__Sources__S3__SecretAccessKey`.  
You would set `B2`'s destination in a similar way: `Destination__Sources__B2__SecretAccessKey`.  

**Command Line Args:**  
The convention is to follow the json path to the property you want to override, nested scopes are traversed with a single colon `:`.  
For example, to set a source's _SecretAccessKey_, the arg would be: `--Destination:Sources:S3:SecretAccessKey=XXX`.  
You would set `B2`'s destination in a similar way: `--Destination:Sources:B2:SecretAccessKey=XXX`.  

**Config Hierarchy:**  
1) Command Line Arguments (_Highest Priority_)
2) Environment Variables
3) appsettings.{stage}.json
4) appsettings.json

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
  },
  "Download": {
    "LocalRootPath": "C:\\wass\\restores\\"
  },
  "Cli": {
    "Options": "--dry-run --destination=S3"
  }
}
```

## Philosophy

WASS does not support file deletes.  
WASS does not support file updates.  

If you would like to remove something from storage, you must do so manually by going to your 3rd party storage console, or local drive interfaces.  
In the same vein, if you update a file to "empty", that's the same as deleting / removing it; so WASS does not allow it.  

Why? Because if you are backing up a file though WASS, it is important to you; and you do not want to lose it.  
We want to avoid replicating source file changes to backup target locations.  
By not implementing delete, or update functionity in WASS, it will be _harder_ for WASS to lose your data though any program bugs; or user actions.  
Ideally any api keys you provide WASS contain create-only semantics, as an extra layer of protection.  

WASS treats files as immuable objects, all changes are handled with additions, not overwrites (_a file is identified by it's hash_).  
When uploading a file though WASS, firstly we search for the hash at the target server.  
If it exists, we do not upload the file data; but we may still upload config, or metadata; and tags (_if they are missing_).  

WASS handles metadata the same way as files - immuable & create-only.  
For example when adding _tags_ for a file, we do not have a tags file that is updated over time.  
Instead new tag files are created under a path stemming from the file's hash value.  

As a side note, this philosophy makes WASS a poor choice for backing up things you are currently working on.  
If you are writing a story let's say, each time you saved it; WASS will treat it as a new file (_since the hash has changed_).  
Over time as you back this up, you might have hundreds of files for your story (_or more!_).  
This gets worse when the work is something larger, for example a photoshop file; or a video that you're editing.  
Therefore, do not keep any working directories under a target location used by WASS for backups.  

## Object Storage

These services have S3 compatible APIs, offer competitive pricing, and are popular community choices (_others are available_):

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

When viewing content from WASS destinations directly, it's a good idea to use a CDN.  
WASS does not have a "viewer" for files (_local or online_), but you can use third-party software (_i.e. [JellyFin](https://jellyfin.org/), [S3Drive](https://s3drive.app/)_).  
WASS does not have any native support for CDNs, however using one on top of a destinations is fine.  
Of course this will only work if the files were originally uploaded in "plaintext" (_i.e. not encrypted / compressed_).  

Here are some popular choices (_others are available_):  

* [Bunny](https://bunny.net/cdn/)
* [Cloudflare](https://www.cloudflare.com/cdn/)
* [Fastly](https://www.fastly.com/products/cdn)
* [Keycdn](https://www.keycdn.com/)
* [Cloudfront](https://aws.amazon.com/cloudfront/)

Note that media (_video_) streaming though CDNs is typically sold as a separate / specialised product.  

# Credits

* [Icon](https://www.flaticon.com/free-icon/bird_2630452) made by [Vitaly Gorbachev](https://www.flaticon.com/authors/vitaly-gorbachev) from [Flaticon](https://www.flaticon.com/).
* [TextCopy](https://github.com/CopyText/TextCopy) made by [Simon Cropp](https://github.com/SimonCropp)

# Changelog

## 0.0.1

* Upload empty project to github.
